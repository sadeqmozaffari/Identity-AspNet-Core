using Microsoft.Extensions.Configuration;
using App.Data.Contracts;
using App.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public AppDBContext _Context { get; }
       // private IMapper _mapper;
      
        private readonly IConfiguration _configuration;

        public UnitOfWork(AppDBContext context,/* IMapper mapper,*/ IConfiguration configuration)
        {
            _Context = context;
            //_mapper = mapper;
            _configuration = configuration;
        }

        public IBaseRepository<TEntity> BaseRepository<TEntity>() where TEntity : class
        {
            IBaseRepository<TEntity> repository = new BaseRepository<TEntity, AppDBContext>(_Context);
            return repository;
        }

    
        public async Task Commit()
        {
            await _Context.SaveChangesAsync();
        }
    }
}
