using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace NumberPrediction
{
    public interface IDocumentDBRepository<T> where T : class
    {
        Task<Document> CreateItemAsync(T item);
        Task DeleteItemAsync(string id);
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, DateTime>> predicate);
        Task<Document> UpdateItemAsync(string id, T item);
    }
}