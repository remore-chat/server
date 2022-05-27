using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTalk.Server.EF;
using TTalk.Server.Models;
using TTalk.Server.Models.Dto;
using TTalk.Server.Utility;

namespace TTalk.Server.Services
{
    public class ConfigurationService
    {
        private ServerDbContext _context;
        private IMapper _mapper;
        private ILogger _logger;

        public ConfigurationService(ServerDbContext context, IMapper mapper, ILogger<TTalkServer> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<ServerConfiguration> GetServerConfigurationAsync()
        {
            var configuration = await _context.Configuration.Include(x => x.PrivilegeKey).AsNoTracking().FirstOrDefaultAsync();
            if (configuration == null)
            {
                configuration = new ServerConfiguration()
                {
                    Name = "Brand new and shiny TTalk Server",
                    MaxClients = 32,
                    PrivilegeKey = null
                };
                var entry = _context.Configuration.Add(configuration);
                _context.Channels.Add(new Channel()
                {
                    Name = "general",
                    ChannelType = Library.Enums.ChannelType.Text,
                    Bitrate = 0,
                    MaxClients = 999999,
                    Order = 0,
                });
                _context.Channels.Add(new Channel()
                {
                    Name = "General",
                    ChannelType = Library.Enums.ChannelType.Voice,
                    Bitrate = 64000,
                    MaxClients = 999999,
                    Order = 1,
                });
                await _context.SaveChangesAsync();
                entry.State = EntityState.Detached;
                await _context.SaveChangesAsync();
                return await _context.Configuration.AsNoTracking().FirstAsync();
            }
            return configuration;
        }

        public async Task<ServerConfiguration> UpdateServerConfigurationAsync(ServerConfiguration updatedConfiguration)
        {
            var config = await GetServerConfigurationAsync();
            if (config?.PrivilegeKey?.Id != updatedConfiguration?.PrivilegeKey?.Id)
                await _context.AddAsync(updatedConfiguration.PrivilegeKey);
            var configuration = _mapper.Map<ServerConfiguration>(_mapper.Map<ServerConfigurationUpdateDto>(updatedConfiguration));
            var entry = _context.Configuration.Update(configuration);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<PrivilegeKey> GeneratePrivilegeKeyAsync()
        {
            var key = new PrivilegeKey();
            var stringKey = RandomStringGenerator.Create(48);
            key.Key = stringKey;
            return key;
        }
    }
}
