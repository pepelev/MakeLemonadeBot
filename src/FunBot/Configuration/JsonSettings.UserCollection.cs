using System.Globalization;
using FunBot.Json;
using Newtonsoft.Json.Linq;

namespace FunBot.Configuration
{
    public sealed partial class JsonSettings
    {
        private sealed class UserCollection : User.Collection
        {
            private readonly JObject @object;

            public UserCollection(JObject @object)
            {
                this.@object = @object;
            }

            public override Configuration.User Get(long chatId)
            {
                var propertyName = chatId.ToString(CultureInfo.InvariantCulture);
                if (@object.ContainsKey(propertyName))
                {
                    return new User(@object.Get<JObject>(propertyName));
                }

                if (@object.ContainsKey("others"))
                {
                    return new User(@object.Get<JObject>("others"));
                }

                return new ConstUser(5);
            }

            private sealed class User : Configuration.User
            {
                private readonly JObject @object;

                public User(JObject @object)
                {
                    this.@object = @object;
                }

                public override int DailyBudget => @object.Get<int>("dailyBudget");
            }
        }
    }
}