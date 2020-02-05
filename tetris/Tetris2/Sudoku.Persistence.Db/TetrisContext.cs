using System;
using System.Data.Entity;

namespace Tetris.Persistence
{
    /// <summary>
    /// Adatbázis kontextus típusa.
    /// </summary>
    /// <seealso cref="System.Data.Entity.DbContext" />
	class TetrisContext : DbContext
	{
		public TetrisContext(String connection)
			: base(connection)
		{
		}

		public DbSet<Game> Games { get; set; }
		public DbSet<Field> Fields { get; set; }
	}
}