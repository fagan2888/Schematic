﻿using System.Collections.Generic;
using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseViewProfile : Profile
    {
        public DatabaseViewProfile()
        {
            CreateMap<Dto.DatabaseView, DatabaseView>()
                .ConstructUsing((dto, ctx) =>
                    dto.IsMaterialized
                        ? new DatabaseMaterializedView(
                            ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.ViewName),
                            dto.Definition,
                            ctx.Mapper.Map<IEnumerable<Dto.DatabaseColumn>, List<DatabaseColumn>>(dto.Columns))
                        : new DatabaseView(
                            ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.ViewName),
                            dto.Definition,
                            ctx.Mapper.Map<IEnumerable<Dto.DatabaseColumn>, List<DatabaseColumn>>(dto.Columns)))
                .ForAllMembers(cfg => cfg.Ignore());

            CreateMap<IDatabaseView, Dto.DatabaseView>()
                .ForMember(dest => dest.ViewName, src => src.MapFrom(v => v.Name));
        }
    }
}
