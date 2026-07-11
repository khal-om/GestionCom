using GestionCom.Data;
using Microsoft.EntityFrameworkCore;
using GestionCom.DTOs.Client;
using GestionCom.Entities;

namespace GestionCom.Services
{
    public class ClientService
    {
        private readonly AppDbContext _context;

        public ClientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateClientAsync(CreateClientDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Le nom du client est requis.");

            var client = new Client
            {
                Name = dto.Name.Trim(),
                Profession = Normalize(dto.Profession),
                MatriculeFiscale = Normalize(dto.MatriculeFiscale),
                Email = Normalize(dto.Email),
                Telephone = Normalize(dto.Telephone),
                Portable = Normalize(dto.Portable),
                Fax = Normalize(dto.Fax),
                Adresse = Normalize(dto.Adresse),
                CodePostal = Normalize(dto.CodePostal),
                Ville = Normalize(dto.Ville),
                Pays = Normalize(dto.Pays),
                Banque = Normalize(dto.Banque),
                RIB = Normalize(dto.RIB),
                Note = Normalize(dto.Note),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client.Id;
        }

        public async Task UpdateClientAsync(int clientId, CreateClientDto dto, int userId, bool isAdmin = false)
        {
            if (!isAdmin)
                throw new UnauthorizedAccessException("Seuls les admins peuvent modifier un client.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Le nom du client est requis.");

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
                throw new InvalidOperationException("Client introuvable.");

            // Update fields
            client.Name = dto.Name.Trim();
            client.Profession = Normalize(dto.Profession);
            client.MatriculeFiscale = Normalize(dto.MatriculeFiscale);
            client.Email = Normalize(dto.Email);
            client.Telephone = Normalize(dto.Telephone);
            client.Portable = Normalize(dto.Portable);
            client.Fax = Normalize(dto.Fax);
            client.Adresse = Normalize(dto.Adresse);
            client.CodePostal = Normalize(dto.CodePostal);
            client.Ville = Normalize(dto.Ville);
            client.Pays = Normalize(dto.Pays);
            client.Banque = Normalize(dto.Banque);
            client.RIB = Normalize(dto.RIB);
            client.Note = Normalize(dto.Note);

            client.UpdatedAt = DateTime.UtcNow;
            client.UpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }
    }
}
