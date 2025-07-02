/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.Utils
{
    public interface ITool
    {
        string Name { get; }
        
        string Category { get; }

        bool IsEnabled { get; set; }

        void Main(Logger logger);
        
        string Description { get; }
    }
} 