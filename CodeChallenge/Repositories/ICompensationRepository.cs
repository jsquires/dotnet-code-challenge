using CodeChallenge.Models;
using System;
using System.Threading.Tasks;

namespace CodeChallenge.Repositories
{
    public interface ICompensationRepository
    {
        Compensation GetByEmployeeId(String id);
        Compensation Add(Compensation compensation);
        Compensation Remove(Compensation compensation);
        Task SaveAsync();
    }
}