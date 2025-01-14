using SFA.DAS.AODP.Application.MemoryCache;

namespace SFA.DAS.AODP.Application.Repository;

/// <summary>
/// A generic cache based repository implementation that handles CRUD operations for any type of entity.
/// It supports caching using the <see cref="ICacheManager"/> and manages entity data stored in memory.
/// </summary>
/// <typeparam name="T">The type of the entity being handled by the repository. The entity should have an "Id" property of type <see cref="Guid"/>.</typeparam>
public class CachedGenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ICacheManager _cacheManager;  // Instance of ICacheManager used for caching.
    private readonly string _cacheKey;            // Cache key used to store and retrieve entities from the cache.

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRepository{T}"/> class.
    /// </summary>
    /// <param name="cacheManager">The cache manager used to handle caching of entities.</param>
    public CachedGenericRepository(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        _cacheKey = typeof(T).Name + "s"; // Generate a cache key by pluralizing the entity name, e.g., "Forms", "Sections".
    }

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/> from the cache.
    /// If no cached data is found, an empty list is returned.
    /// </summary>
    /// <returns>A collection of all entities of type <typeparamref name="T"/>.</returns>
    public IEnumerable<T> GetAll()
    {
        // Attempt to retrieve the list of entities from the cache.
        return _cacheManager.Get<List<T>>(_cacheKey) ?? new List<T>(); // Return empty list if cache is not found.
    }

    /// <summary>
    /// Retrieves an entity by its unique identifier from the cache.
    /// </summary>
    /// <param name="id">The unique identifier (Guid) of the entity.</param>
    /// <returns>The entity with the specified identifier, or null if not found.</returns>
    public T? GetById(Guid id)
    {
        // Use LINQ to find the entity with the matching ID in the cached list.
        return GetAll().FirstOrDefault(x => ((dynamic)x).Id == id);
    }

    /// <summary>
    /// Adds a new entity to the cache. If the entity has an "Id" property of type <see cref="Guid"/>,
    /// a new Guid is generated and assigned to the entity's Id if it is not already set.
    /// </summary>
    /// <param name="entity">The entity to be added to the cache.</param>
    public void Add(T entity)
    {
        // Use reflection to check if the entity has an "Id" property of type Guid.
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(Guid))
        {
            var idValue = (Guid)idProperty.GetValue(entity);
            // If the Id is empty (Guid.Empty), assign a new GUID.
            if (idValue == Guid.Empty)
            {
                idProperty.SetValue(entity, Guid.NewGuid());
            }
        }

        // Retrieve the current list of entities from the cache, or create a new list if none exists.
        var entities = GetAll().ToList();
        entities.Add(entity); // Add the new entity to the list.
        _cacheManager.Set(_cacheKey, entities); // Store the updated list back in the cache.
    }

    /// <summary>
    /// Updates an existing entity in the cache. The entity is identified by its unique identifier, which is assumed to be of type <see cref="Guid"/>.
    /// </summary>
    /// <param name="entity">The updated entity.</param>
    public bool Update(T entity)
    {
        // Retrieve the list of entities from the cache.
        var entities = GetAll().ToList();
        // Find the index of the entity to be updated based on its "Id" property.
        var index = entities.FindIndex(e => ((dynamic)e).Id == ((dynamic)entity).Id);
        if (index != -1)
        {
            // Replace the entity at the found index with the updated entity.
            entities[index] = entity;
            _cacheManager.Set(_cacheKey, entities); // Store the updated list back in the cache.

            return true;
        }

        //Unable to update
        return false;
    }

    /// <summary>
    /// Deletes an entity from the cache by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (Guid) of the entity to be deleted.</param>
    public bool Delete(Guid id)
    {
        //Assume failure
        var status = false;

        // Retrieve the list of entities from the cache.
        var entities = GetAll().ToList();
        // Find the entity to be deleted based on its "Id" property.
        var entity = entities.FirstOrDefault(e => ((dynamic)e).Id == id);
        if (entity != null)
        {
            // Remove the found entity from the list.
            status = entities.Remove(entity);
            _cacheManager.Set(_cacheKey, entities); // Store the updated list back in the cache.
        }

        return status;
    }
}
