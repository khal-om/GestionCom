using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using GestionCom.DTOs.Settings;
using GestionCom.DTOs.Vente;
using GestionCom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionCom.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/ventes")]
    public class VenteDocumentsController : ControllerBase
    {
        private readonly VenteQueryService _venteQueryService;
        private readonly AppSettingsService _appSettingsService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public VenteDocumentsController(
            VenteQueryService venteQueryService,
            AppSettingsService appSettingsService,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _venteQueryService = venteQueryService;
            _appSettingsService = appSettingsService;
            _environment = environment;
            _configuration = configuration;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpGet("{venteId:int}/document")]
        public async Task<IActionResult> DownloadDocument(int venteId)
            => await DownloadFacture(venteId);

        [HttpGet("{venteId:int}/bon-sortie")]
        public async Task<IActionResult> DownloadBonSortie(int venteId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var vente = await _venteQueryService.GetVenteDetailAsync(venteId, userId, User.IsInRole("Admin"));
                var settings = await _appSettingsService.GetAsync();
                var logoData = await LoadLogoAsync();
                var pdf = BuildBonSortiePdfDocument(vente, settings, logoData).GeneratePdf();

                return File(pdf, "application/pdf", $"BonSortie-{BuildBonSortieNumber(vente)}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{venteId:int}/facture")]
        public async Task<IActionResult> DownloadFacture(int venteId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var vente = await _venteQueryService.GetVenteDetailAsync(venteId, userId, User.IsInRole("Admin"));
                var settings = await _appSettingsService.GetAsync();
                var logoData = await LoadLogoAsync();
                var pdf = BuildFacturePdfDocument(vente, settings, logoData).GeneratePdf();

                return File(pdf, "application/pdf", $"Facture-{BuildFactureNumber(vente)}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private Document BuildBonSortiePdfDocument(VenteDetailDto vente, AppSettingsDto settings, byte[]? logoData)
        {
            var companyInfo = BuildCompanyInfo(settings);
            var bonSortieNumber = BuildBonSortieNumber(vente);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#334155"));

                    // Show header only on the first page
                    page.Header().ShowOnce().Element(header => ComposeHeader(header, logoData, companyInfo, "BON DE SORTIE"));
                    page.Content().PaddingTop(8).Element(content => ComposeBonSortieContent(content, vente, bonSortieNumber));
                    // Footer with company info and page numbering
                    page.Footer().Element(footer => footer.Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Text(companyInfo.FooterLine).FontSize(9);
                        row.ConstantItem(120).AlignRight().Text(text =>
                        {
                            text.Span("Page ").SemiBold().FontSize(9);
                            text.CurrentPageNumber().FontSize(9);
                            text.Span(" / ").FontSize(9);
                            text.TotalPages().FontSize(9);
                        });
                    }));
                });
            });
        }

        private Document BuildFacturePdfDocument(VenteDetailDto vente, AppSettingsDto settings, byte[]? logoData)
        {
            var companyInfo = BuildCompanyInfo(settings);
            var factureNumber = BuildFactureNumber(vente);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#334155"));

                    // Header only on first page to save space on subsequent pages
                    page.Header().ShowOnce().Element(header => ComposeHeader(header, logoData, companyInfo, "FACTURE"));
                    // Reduce top padding to save paper
                    page.Content().PaddingTop(8).Element(content => ComposeFactureContent(content, vente, factureNumber));
                    // Footer with company info and page numbers
                    page.Footer().Element(footer => footer.Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Text(companyInfo.FooterLine).FontSize(9);
                        row.ConstantItem(120).AlignRight().Text(text =>
                        {
                            text.Span("Page ").SemiBold().FontSize(9);
                            text.CurrentPageNumber().FontSize(9);
                            text.Span(" / ").FontSize(9);
                            text.TotalPages().FontSize(9);
                        });
                    }));
                });
            });
        }

        private static void ComposeHeader(IContainer container, byte[]? logoData, CompanyDocumentInfo companyInfo, string documentTitle)
        {
            container
                .BorderBottom(1)
                .BorderColor("#d6d3d1")
                .PaddingBottom(14)
                .Row(row =>
                {
                    row.RelativeItem().Row(left =>
                    {
                        left.ConstantItem(82).Height(82).Element(box => ComposeLogo(box, logoData));
                        left.RelativeItem().PaddingLeft(14).Column(column =>
                        {
                            column.Item().Text(documentTitle).FontSize(9).SemiBold().FontColor("#78716c");
                            column.Item().PaddingTop(6).Text(companyInfo.Name).FontSize(20).Bold().FontColor("#0f172a");
                            if (!string.IsNullOrWhiteSpace(companyInfo.Tagline))
                            {
                                column.Item().Text(companyInfo.Tagline);
                            }
                        });
                    });

                    row.ConstantItem(220).AlignRight().Column(column =>
                    {
                        column.Item().AlignRight().Text(companyInfo.Address);
                        column.Item().AlignRight().Text(companyInfo.Phone);
                        column.Item().AlignRight().Text(companyInfo.Email);
                    });
                });
        }

        private static void ComposeLogo(IContainer container, byte[]? logoData)
        {
            var logoBox = container
                .Background("#ffffff")
                .Border(1)
                .BorderColor("#d6d3d1")
                .CornerRadius(12)
                .AlignCenter()
                .AlignMiddle();

            if (logoData is not null)
            {
                logoBox.Image(logoData);
            }
            else
            {
                logoBox.Text("Logo").SemiBold().FontColor("#475569");
            }
        }

        private static void ComposeBonSortieContent(IContainer container, VenteDetailDto vente, string bonSortieNumber)
        {
            container.Column(column =>
            {
                column.Spacing(16);

                column.Item().Element(section => ComposeDocumentIdentity(section, "Bon de sortie n°", bonSortieNumber, vente.DateVente));
                column.Item().Element(section => ComposeBonSortieLines(section, vente));
            });
        }

        private static void ComposeFactureContent(IContainer container, VenteDetailDto vente, string factureNumber)
        {
            var totalHt = GetTotalHt(vente);
            var totalTva = decimal.Round(totalHt * 0.19m, 3, MidpointRounding.AwayFromZero);
            const decimal timbre = 1.000m;
            var totalTtc = totalHt + totalTva + timbre;

            container.Column(column =>
            {
                column.Spacing(16);
                column.Item().Element(section => ComposeDocumentIdentity(section, "Facture n°", factureNumber, vente.DateVente));
                column.Item().Element(section => ComposeFactureClient(section, vente));
                column.Item().Element(section => ComposeFactureLines(section, vente));
                column.Item().AlignRight().Width(260).Element(section => ComposeFactureTotals(section, totalHt, totalTva, timbre, totalTtc));
            });
        }

        private static void ComposeDocumentIdentity(IContainer container, string label, string number, DateTime date)
        {
            container.Row(row =>
            {
                row.Spacing(12);
                row.RelativeItem().Element(card => ComposeInlineInfoCard(card, label, number));
                row.RelativeItem().Element(card => ComposeInlineInfoCard(card, "Date", date.ToString("dd/MM/yyyy")));
            });
        }

        private static void ComposeBonSortieLines(IContainer container, VenteDetailDto vente)
        {
            ComposeSection(container, "Produits de la vente", section =>
            {
                section.Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        ComposeTableHeaderCell(header.Cell(), "Réf");
                        ComposeTableHeaderCell(header.Cell(), "Produit");
                        ComposeTableHeaderCell(header.Cell(), "Quantité", alignRight: true);
                    });

                    if (vente.Lignes.Count == 0)
                    {
                        table.Cell().ColumnSpan(3).Padding(10).Text("Aucun article.");
                    }
                    else
                    {
                        foreach (var line in vente.Lignes)
                        {
                            ComposeTableBodyCell(table.Cell(), line.ProduitId.ToString(CultureInfo.InvariantCulture));
                            ComposeTableBodyCell(table.Cell(), line.ProduitLabel);
                            ComposeTableBodyCell(table.Cell(), line.Quantite.ToString(CultureInfo.InvariantCulture), alignRight: true);
                        }
                    }
                });
            });
        }

        private static void ComposeFactureClient(IContainer container, VenteDetailDto vente)
        {
            ComposeSection(container, null, section =>
            {
                section.Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Row(row =>
                    {
                        row.Spacing(12);
                        row.RelativeItem().Element(item => ComposeInlineMetaItem(item, "Nom", vente.ClientName));
                        row.RelativeItem().Element(item => ComposeInlineMetaItem(item, "Téléphone", DisplayOrFallback(vente.ClientTelephone ?? vente.ClientPortable, "Non renseigné")));
                    });
                    column.Item().Row(row =>
                    {
                        row.Spacing(12);
                        row.RelativeItem().Element(item => ComposeInlineMetaItem(item, "Adresse", FormatAddress(vente)));
                        row.RelativeItem().Element(item => ComposeInlineMetaItem(item, "MF", DisplayOrFallback(vente.ClientMatriculeFiscale, "Non renseigné")));
                    });
                });
            });
        }

        private static void ComposeFactureLines(IContainer container, VenteDetailDto vente)
        {
            ComposeSection(container, null, section =>
            {
                section.Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(3.3f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1.8f);
                    });

                    table.Header(header =>
                    {
                        ComposeTableHeaderCell(header.Cell(), "Réf");
                        ComposeTableHeaderCell(header.Cell(), "Nom de produit");
                        ComposeTableHeaderCell(header.Cell(), "Quantité", alignRight: true);
                        ComposeTableHeaderCell(header.Cell(), "PU", alignRight: true);
                        ComposeTableHeaderCell(header.Cell(), "Remise", alignRight: true);
                        ComposeTableHeaderCell(header.Cell(), "TVA", alignRight: true);
                        ComposeTableHeaderCell(header.Cell(), "Total HT", alignRight: true);
                    });

                    if (vente.Lignes.Count == 0)
                    {
                        table.Cell().ColumnSpan(7).Padding(10).Text("Aucun article.");
                    }
                    else
                    {
                        foreach (var line in vente.Lignes)
                        {
                            ComposeTableBodyCell(table.Cell(), line.ProduitId.ToString(CultureInfo.InvariantCulture));
                            ComposeTableBodyCell(table.Cell(), line.ProduitLabel);
                            ComposeTableBodyCell(table.Cell(), line.Quantite.ToString(CultureInfo.InvariantCulture), alignRight: true);
                            ComposeTableBodyCell(table.Cell(), FormatCompactCurrency(line.PrixUnitaire), alignRight: true);
                            ComposeTableBodyCell(table.Cell(), FormatPercent(line.RemisePercent), alignRight: true);
                            ComposeTableBodyCell(table.Cell(), "19 %", alignRight: true);
                            ComposeTableBodyCell(table.Cell(), FormatCompactCurrency(line.TotalLigne ?? 0m), alignRight: true);
                        }
                    }
                });
            });
        }

        private static void ComposeInlineInfoCard(IContainer container, string label, string value)
        {
            container
                .Background("#ffffff")
                .Border(1)
                .BorderColor("#d6d3d1")
                .CornerRadius(12)
                .PaddingHorizontal(12)
                .PaddingVertical(10)
                .Text(text =>
                {
                    text.Span(label + ": ").FontSize(9).SemiBold().FontColor("#78716c");
                    text.Span(value).FontSize(12).SemiBold().FontColor("#0f172a");
                });
        }

        private static void ComposeFactureTotals(IContainer container, decimal totalHt, decimal totalTva, decimal timbre, decimal totalTtc)
        {
            container
                .Background("#eff6ff")
                .Border(1)
                .BorderColor("#bfdbfe")
                .CornerRadius(12)
                .Padding(12)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.4f);
                    });

                    AddSummaryRow(table, "Total HT", totalHt, false);
                    AddSummaryRow(table, "Total TVA", totalTva, false);
                    AddSummaryRow(table, "Timbre", timbre, false);
                    AddSummaryRow(table, "Total TTC", totalTtc, true);
                });
        }

        private static void ComposeSection(IContainer container, string? title, Action<IContainer> content)
        {
            container
                .Background("#ffffff")
                .Border(1)
                .BorderColor("#d6d3d1")
                .CornerRadius(14)
                .Padding(14)
                .Column(column =>
                {
                    column.Spacing(string.IsNullOrWhiteSpace(title) ? 0 : 10);
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        column.Item().Text(title).FontSize(9).SemiBold().FontColor("#78716c");
                    }
                    column.Item().Element(content);
                });
        }

        private static void ComposeInfoCard(IContainer container, string label, string value)
        {
            container
                .Background("#ffffff")
                .Border(1)
                .BorderColor("#d6d3d1")
                .CornerRadius(12)
                .Padding(12)
                .Column(column =>
                {
                    column.Item().Text(label).FontSize(8).SemiBold().FontColor("#78716c");
                    column.Item().PaddingTop(4).Text(value).FontSize(12).SemiBold().FontColor("#0f172a");
                });
        }

        private static void ComposeCompactInfoCard(IContainer container, string label, string value)
        {
            container
                .Background("#ffffff")
                .Border(1)
                .BorderColor("#d6d3d1")
                .CornerRadius(12)
                .PaddingHorizontal(10)
                .PaddingVertical(8)
                .Column(column =>
                {
                    column.Item().Text(label).FontSize(7).SemiBold().FontColor("#78716c");
                    column.Item().PaddingTop(2).Column(valueColumn =>
                    {
                        valueColumn.Spacing(1);

                        foreach (var line in value.Split('\n'))
                        {
                            valueColumn.Item().Text(line).FontSize(11).SemiBold().FontColor("#0f172a");
                        }
                    });
                });
        }

        private static void ComposeMetaItem(IContainer container, string label, string value)
        {
            container.Column(column =>
            {
                column.Spacing(2);
                column.Item().Text(label).FontSize(8).SemiBold().FontColor("#78716c");
                column.Item().Text(value).FontSize(10).FontColor("#334155");
            });
        }

        private static void ComposeInlineMetaItem(IContainer container, string label, string value)
        {
            container.Text(text =>
            {
                text.Span(label + ": ").FontSize(9).SemiBold().FontColor("#78716c");
                text.Span(value).FontSize(10).FontColor("#334155");
            });
        }

        private static void ComposeTableHeaderCell(IContainer container, string text, bool alignRight = false)
        {
            var cell = container
                .Background("#f5f5f4")
                .BorderBottom(1)
                .BorderColor("#d6d3d1")
                .Padding(6);

            if (alignRight)
            {
                cell = cell.AlignRight();
            }

            cell.Text(text)
                .FontSize(8)
                .SemiBold()
                .FontColor("#78716c");
        }

        private static void ComposeTableBodyCell(IContainer container, string text, bool alignRight = false)
        {
            var cell = container
                .BorderBottom(1)
                .BorderColor("#e7e5e4")
                .PaddingVertical(4)
                .PaddingHorizontal(6);

            if (alignRight)
            {
                cell = cell.AlignRight();
            }

            cell.Text(text)
                .FontSize(9)
                .FontColor("#334155");
        }

        private static void AddSummaryRow(TableDescriptor table, string label, decimal amount, bool emphasize)
        {
            var labelCell = table.Cell().BorderBottom(1).BorderColor("#dbeafe").PaddingVertical(6).PaddingHorizontal(8).AlignRight();
            var valueCell = table.Cell().BorderBottom(1).BorderColor("#dbeafe").PaddingVertical(6).PaddingHorizontal(8).AlignRight();

            if (emphasize)
            {
                labelCell.Text(label).Bold().FontColor("#0f172a");
                valueCell.Text(FormatCurrency(amount)).Bold().FontColor("#1d4ed8");
            }
            else
            {
                labelCell.Text(label).SemiBold().FontColor("#475569");
                valueCell.Text(FormatCurrency(amount)).SemiBold().FontColor("#0f172a");
            }
        }

        private async Task<byte[]?> LoadLogoAsync()
        {
            var configuredLogoPath = _configuration["Branding:Logo"] ?? "/branding/logo.jpeg";
            var relativeLogoPath = configuredLogoPath.TrimStart('/', '\\')
                .Replace('/', Path.DirectorySeparatorChar);
            var logoPath = Path.Combine(_environment.WebRootPath, relativeLogoPath);
            return System.IO.File.Exists(logoPath)
                ? await System.IO.File.ReadAllBytesAsync(logoPath)
                : null;
        }

        private static string DisplayOrFallback(string? value, string fallback)
            => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

        private static string DisplayDate(DateTime? value)
            => value.HasValue ? value.Value.ToString("dd/MM/yyyy") : "—";

        private static CompanyDocumentInfo BuildCompanyInfo(AppSettingsDto settings)
        {
            var name = DisplayOrFallback(settings.CompanyName, "FedNad Holding");
            var tagline = string.IsNullOrWhiteSpace(settings.CompanyTagline) ? null : settings.CompanyTagline.Trim();
            var address = DisplayOrFallback(settings.CompanyAddress, "Adresse de l'entreprise");
            var phone = DisplayOrFallback(settings.CompanyPhone, "Téléphone de l'entreprise");
            var email = DisplayOrFallback(settings.SupportEmail, "email@entreprise.com");
            var footerLine = string.Join(" | ", new[] { name, address, phone }.Where(v => !string.IsNullOrWhiteSpace(v)));

            return new CompanyDocumentInfo(name, tagline, address, phone, email, footerLine);
        }

        private static string FormatAddress(VenteDetailDto vente)
        {
            var parts = new[]
            {
                vente.ClientAddress,
                string.Join(" ", new[] { vente.ClientPostalCode, vente.ClientCity }.Where(v => !string.IsNullOrWhiteSpace(v))),
                vente.ClientCountry
            };

            var value = string.Join(", ", parts.Where(v => !string.IsNullOrWhiteSpace(v)));
            return string.IsNullOrWhiteSpace(value) ? "Non renseignée" : value;
        }

        private static string FormatCurrency(decimal? amount)
            => amount.HasValue ? amount.Value.ToString("N3", CultureInfo.GetCultureInfo("fr-FR")) + " TND" : "—";

        private static string FormatCompactCurrency(decimal? amount)
            => amount.HasValue ? amount.Value.ToString("N3", CultureInfo.GetCultureInfo("fr-FR")) : "—";

        private static string FormatPercent(decimal value)
            => value.ToString("N2", CultureInfo.InvariantCulture) + " %";

        private static string BuildBonSortieNumber(VenteDetailDto vente)
        {
            var randomLikeSuffix = Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(vente.DocumentNumber))[^6..];
            return $"BS-{vente.DateVente:dd-MM-yy}-{randomLikeSuffix}";
        }

        private static string BuildFactureNumber(VenteDetailDto vente)
        {
            var factureNumber = ResolveFactureNumber(vente);

            return $"REV{vente.DateVente:yy}/{factureNumber}";
        }

        private static string ResolveFactureNumber(VenteDetailDto vente)
        {
            var raw = vente.FactureNumber?.Trim();

            if (string.IsNullOrWhiteSpace(raw)
                || string.Equals(raw, vente.DocumentNumber, StringComparison.OrdinalIgnoreCase)
                || Regex.IsMatch(raw, "^V-\\d{2}-\\d{2}-\\d{2}-[A-Z0-9]{5}$"))
            {
                return vente.Id.ToString(CultureInfo.InvariantCulture);
            }

            return raw;
        }

        private static decimal GetTotalHt(VenteDetailDto vente)
            => decimal.Round(vente.Lignes.Sum(line => line.TotalLigne ?? 0m), 3, MidpointRounding.AwayFromZero);

        private static string FormatDocumentNumberForDisplay(string documentNumber)
        {
            var lastSeparatorIndex = documentNumber.LastIndexOf('-');

            return lastSeparatorIndex > 0
                ? documentNumber[..lastSeparatorIndex] + "\n" + documentNumber[(lastSeparatorIndex + 1)..]
                : documentNumber;
        }

        private sealed record CompanyDocumentInfo(string Name, string? Tagline, string Address, string Phone, string Email, string FooterLine);
    }
}
