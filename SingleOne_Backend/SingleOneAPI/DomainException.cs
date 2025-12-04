using System;

namespace SingleOne
{
    public class DomainException : Exception
    {
        public DomainException() { }
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class EntidadeJaExisteEx : DomainException
    {
        public EntidadeJaExisteEx(string message) : base(message) { }
    }

    public class EntidadeNaoEncontradaEx : DomainException
    {
        public EntidadeNaoEncontradaEx(string message) : base(message) { }
    }
}
