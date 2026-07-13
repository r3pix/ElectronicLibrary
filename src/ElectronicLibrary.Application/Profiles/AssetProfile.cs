using AutoMapper;
using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Entities;

namespace ElectronicLibrary.Application.Profiles;

public class AssetProfile : Profile
{
    public AssetProfile()
    {
        CreateMap<Asset, AssetModel>();
    }
}
