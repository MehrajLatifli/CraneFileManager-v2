using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Application.Exceptions
{
    public class ConflictException : ApplicationException
    {
        public ConflictException(string message) : base(message) { }

    }

}
