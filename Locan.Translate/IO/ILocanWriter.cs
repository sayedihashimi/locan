namespace Locan.Translate.IO {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface ILocanWriter : IDisposable {
        void WriteRow(ILocanRow row);

        void WriteRows(IEnumerable<ILocanRow> rows);
    }
}
