using System;
using Newtonsoft.Json.Linq;
using Ripple.Core.Binary;

namespace Ripple.Core.Types
{
    public class Amount : ISerializedType
    {
        public readonly AccountId Issuer;
        public readonly Currency Currency;
        public bool IsNative => Currency.IsNative;
        public AmountValue Value;

        public const int MaximumIouPrecision = 16;

        public Amount(AmountValue value,
                      Currency currency=null,
                      AccountId issuer=null)
        {
            Currency = currency ?? Currency.Xrp;
            Issuer = issuer ?? (Currency.IsNative ?
                                    AccountId.Zero :
                                    AccountId.Neutral);
            Value = value;
        }

        public Amount(string v="0", Currency c=null, AccountId i=null) :
                      this(AmountValue.FromString(v, c == null || c.IsNative), c, i)
        {
        }

        public void ToBytes(IBytesSink sink)
        {
            sink.Put(Value.ToBytes());
            if (!IsNative)
            {
                Currency.ToBytes(sink);
                Issuer.ToBytes(sink);
            }
        }

        public JToken ToJson()
        {
            if (IsNative)
            {
                return Value.ToString();
            }
            return new JObject
            {
                ["value"] = Value.ToString(),
                ["currency"] = Currency,
                ["issuer"] = Issuer,
            };
        }

        public static Amount FromJson(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Integer:
                    return (ulong)token;
                case JTokenType.String:
                    return new Amount(token.ToString());
                case JTokenType.Object:
                    string value = null;
                    string currency = null;
                    string issuer = null;

                    // we must allow property names that are not in lowercase
                    foreach(var child in token.Children())
                    {
                        var p = child as JProperty;
                        if (p == null)
                            throw new InvalidJsonException("Invalid amount object.");

                        if (p.Name.Equals("value", StringComparison.OrdinalIgnoreCase))
                            value = p.Value.ToString();
                        else if (p.Name.Equals("currency", StringComparison.OrdinalIgnoreCase))
                            currency = p.Value.ToString();
                        else if (p.Name.Equals("issuer", StringComparison.OrdinalIgnoreCase))
                            issuer = p.Value.ToString();
                        else
                            throw new InvalidJsonException($"Unexpected field `{p.Name}`");
                    }

                    if (value == null)
                        throw new InvalidJsonException("Amount object must contain property `value`");

                    if(currency == null)
                        throw new InvalidJsonException("Amount object must contain property `currency`");

                    if (issuer == null)
                        throw new InvalidJsonException("Amount object must contain property `issuer`");

                    return new Amount(value.ToString(), currency, issuer);
                default:
                    throw new InvalidJsonException("Can not create Amount from `{token}`");
            }
        }

        public static implicit operator Amount(ulong a)
        {
            return new Amount(a.ToString("D"));
        }

        public static implicit operator Amount(string v)
        {
            return new Amount(v);
        }

        public static Amount FromParser(BinaryParser parser, int? hint=null)
        {
            var value = AmountValue.FromParser(parser);
            if (!value.IsIou) return new Amount(value);
            var curr = Currency.FromParser(parser);
            var issuer = AccountId.FromParser(parser);
            return new Amount(value, curr, issuer);
        }
    }
}