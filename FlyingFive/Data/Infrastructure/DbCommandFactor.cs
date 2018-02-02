using FlyingFive.Data.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    public class DbCommandFactor
    {
        public DbCommandFactor(IObjectActivator objectActivator, string commandText, FakeParameter[] parameters)
        {
            this.ObjectActivator = objectActivator;
            this.CommandText = commandText;
            this.Parameters = parameters;
        }
        public IObjectActivator ObjectActivator { get; set; }
        public string CommandText { get; set; }
        public FakeParameter[] Parameters { get; set; }
    }
}
