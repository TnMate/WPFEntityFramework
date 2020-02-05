using System;
using System.ComponentModel.DataAnnotations;

namespace Tetris.Persistence
{
    /// <summary>
    /// Mező entitás típusa.
    /// </summary>
    class Field
	{
        /// <summary>
        /// Egyedi azonosító.
        /// </summary>
        [Key]
		public Int32 Id { get; set; }

	    /// <summary>
	    /// Vízszintes koordináta.
	    /// </summary>
		public Int32 X { get; set; }
	    /// <summary>
	    /// Függőleges koordináta.
	    /// </summary>
		public Int32 Y { get; set; }
	    /// <summary>
	    /// Tárolt érték.
	    /// </summary>
		public Byte Value { get; set; }
	    /// <summary>
	    /// típus tulajdonság lekérdezése.
	    /// </summary>
		public Int32 Type { get; set; }

        /// <summary>
        /// Kapcsolt játék.
        /// </summary>
        public Game Game { get; set; }
	}
}