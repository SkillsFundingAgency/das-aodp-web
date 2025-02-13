using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Repository;
/// <summary>
/// Defines the contract for a generic repository that provides CRUD operations for entities of any type.
/// This interface allows working with entities stored in-memory (via caching in the implementation).
/// </summary>
/// <typeparam name="T">The type of the entity the repository works with. The entity should have an "Id" property of type <see cref="Guid"/>.</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>A collection of all entities of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method will retrieve all entities stored in the repository, typically from the cache.
    /// If no entities are found, an empty collection will be returned.
    /// </remarks>
    IEnumerable<T> GetAll();

    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (Guid) of the entity.</param>
    /// <returns>The entity matching the provided identifier, or null if no matching entity is found.</returns>
    /// <remarks>
    /// This method fetches an entity based on its unique identifier from the repository (cache).
    /// If no entity with the specified identifier is found, it returns null.
    /// </remarks>
    T? GetById(Guid id);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add to the repository.</param>
    /// <remarks>
    /// This method inserts a new entity into the repository (typically caching it in memory).
    /// If the entity does not have an "Id" set, a new <see cref="Guid"/> will be generated for it.
    /// </remarks>
    void Add(T entity);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity with updated data.</param>
    /// <remarks>
    /// This method updates an existing entity in the repository based on its "Id".
    /// If the entity exists, it will be replaced with the updated data.
    /// </remarks>
    bool Update(T entity);

    /// <summary>
    /// Deletes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (Guid) of the entity to delete.</param>
    /// <remarks>
    /// This method removes the entity from the repository (cache) based on its "Id".
    /// If the entity is found, it will be deleted, and the repository will be updated.
    /// </remarks>
    bool Delete(Guid id);
}

