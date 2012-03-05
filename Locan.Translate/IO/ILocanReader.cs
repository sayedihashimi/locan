namespace Locan.Translate.IO {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface ILocanReader : IDisposable {
        IEnumerable<ILocanRow> GetRowsToBeTranslated();
        IEnumerable<ILocanRow> GetRowsTranslated();
    }

    
}
