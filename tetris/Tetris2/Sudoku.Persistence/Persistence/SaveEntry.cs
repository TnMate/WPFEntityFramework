using System;

namespace Tetris.Persistence
{
    /// <summary>
    /// Játékállapot mentés reprezentációja.
    /// </summary>
    public class SaveEntry
	{
        /// <summary>
        /// Mentés neve vagy elérési útja.
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// Mentés időpontja.
        /// </summary>
        public DateTime Time { get; set; }
	}
}