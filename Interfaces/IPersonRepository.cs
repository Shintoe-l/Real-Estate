using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstate.Models;

namespace RealEstate.Interfaces
{
    public interface IPersonRepository
    {
        Task<List<Person>> GetAllAsync();
        Task<Person?> GetByIdAsync(Guid id);
        Task<Person?> GetByEmailAsync(string email);
        Task<Person> CreateAsync(Person person);
        Task<Person?> UpdateAsync(Guid id, Person person);
        Task<Person?> DeleteAsync(Guid id);
        Task<bool> PersonExistsAsync(Guid id);
    }
}
