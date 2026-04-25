using GestionCom.Data;
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
