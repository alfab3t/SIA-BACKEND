using astratech_apps_backend.DTOs.Institusi;
using astratech_apps_backend.Models;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IInstitusiRepository
    {
        Task<(IEnumerable<Institusi>, int totalData)> GetAllAsync(GetAllInstitusiRequest dto);
        Task<Institusi?> GetByIdAsync(short id);
        Task<int> CreateAsync(CreateInstitusiRequest dto, string createdBy);
        Task<bool> UpdateAsync(UpdateInstitusiRequest dto, string updatedBy);
        Task<bool> SetStatusAsync(short id, string updatedBy);
    }
}
