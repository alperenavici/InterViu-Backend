namespace Interviu.Service.Exceptions;

public class EntityNotFoundException:Exception
{
    public EntityNotFoundException(string entityName, object id)
        : base($"'{entityName}' entity with ID '{id}' was not found.")
    {
    }

}