using AutoMapper;

namespace Crud.Models;

public interface ICrudDbContext
{
    IMapper Mapper { get; }
}