using Cecil.AspectN.Patterns;
using Cecil.AspectN.Patterns.Parsers;
using Cecil.AspectN.Tokens;

namespace Pooling.Fody.AspectN.Patterns.Parsers
{
    internal class ForceResetTypePrefixParser : ITypePrefixParser
    {
        public bool IsMatch(Token token) => token.Value == '~';

        public IIntermediateTypePattern ParseType(TokenSource tokens)
        {
            return new ForceResetTypePattern(Parser.ParseType(tokens));
        }
    }
}
