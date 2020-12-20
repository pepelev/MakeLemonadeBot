using FunBot.Json;
using Newtonsoft.Json.Linq;

namespace FunBot.Configuration
{
    public sealed partial class JsonSettings
    {
        private sealed class SourceCollection : Source.Collection
        {
            private readonly JObject @object;

            public SourceCollection(JObject @object)
            {
                this.@object = @object;
            }

            public override bool Contains(string name) => @object.ContainsKey(name);
            public override Configuration.Source Get(string name) => new Source(@object.Get<JObject>(name));

            private sealed class Source : Configuration.Source
            {
                private readonly JObject @object;

                public Source(JObject @object)
                {
                    this.@object = @object;
                }

                public override string SpreadsheetId => @object.Get<string>("spreadsheetId");
                public override string Sheet => @object.Get<string>("sheet");
            }
        }
    }
}