// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.ResxTranslator
{
    static class GuidList
    {
        public const string guidResxTranslatorPkgString = "7947748d-4a20-4f26-b34c-1ae043347a34";
        public const string guidResxTranslatorCmdSetString = "eead0768-bf40-4db1-9522-cb58faad805a";
        public const string guidResxTranslatorCrowdSourceString = "eead0768-bf40-4db1-9522-cb58faad805b";

        public static readonly Guid guidResxTranslatorCmdSet = new Guid(guidResxTranslatorCmdSetString);
        public static readonly Guid guidResxTranslatorCrowdSource = new Guid(guidResxTranslatorCrowdSourceString);
    };
}