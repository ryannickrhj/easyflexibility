using EasyFlexibilityTool.Data.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyFlexibilityTool.Data.Model
{
    public class UserProgram : BaseEntity
    {
        public string UserId { get; set; }
        public string ProgramKind { get; set; }
        public string ProgramTitle { get; set; }
    }
}
