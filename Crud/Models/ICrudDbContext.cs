using AutoMapper;

namespace Juulsgaard.Crud.Models;

public interface ICrudDbContext
{
    IMapper Mapper { get; }
}