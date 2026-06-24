using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Interfaces;
using RealEstate.Models;

namespace RealEstate.Repositories
{
    public class PersonRepository(AppDbContext _context) : IPersonRepository
    {
        public async Task<List<Person>> GetAllAsync()
        {
            return await _context.Persons.ToListAsync();
        }

        public async Task<Person?> GetByIdAsync(Guid id)
        {
            return await _context.Persons.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Person?> GetByEmailAsync(string email)
        {
            return await _context.Persons.FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());
        }

        public async Task<Person> CreateAsync(Person person)
        {
            await _context.Persons.AddAsync(person);
            await _context.SaveChangesAsync();
            return person;
        }

        public async Task<Person?> UpdateAsync(Guid id, Person person)
        {
            var existing = await _context.Persons.FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return null;

            existing.FirstName = person.FirstName;
            existing.LastName = person.LastName;
            existing.Email = person.Email;
            existing.PhoneNumber = person.PhoneNumber;
            existing.Role = person.Role;
            if (!string.IsNullOrEmpty(person.PasswordHash))
            {
                existing.PasswordHash = person.PasswordHash;
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Person?> DeleteAsync(Guid id)
        {
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.Id == id);
            if (person == null) return null;

            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();
            return person;
        }

        public async Task<bool> PersonExistsAsync(Guid id)
        {
            return await _context.Persons.AnyAsync(p => p.Id == id);
        }
    }
}
