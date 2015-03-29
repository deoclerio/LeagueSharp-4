#region

using LeagueSharp.Common;

#endregion

namespace Godyr
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += a => Udyr.Load();
        }
    }
}