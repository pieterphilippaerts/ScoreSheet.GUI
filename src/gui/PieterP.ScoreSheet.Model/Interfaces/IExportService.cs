using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.Shared.Interfaces {
    public interface IExportService {
        bool ToXpsFile(IEnumerable<object> documents, string file, bool isLandscape = true);
        bool ToPdfFile(IEnumerable<object> documents, string file, bool isLandscape = true);
        bool ToPrinter(IEnumerable<object> documents, bool isLandscape = true, int? numCopies = null);
    }
}
