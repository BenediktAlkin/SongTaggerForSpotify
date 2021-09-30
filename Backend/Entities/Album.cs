namespace Backend.Entities
{
    public class Album
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string ReleaseDate { get; set; }
        public string ReleaseDatePrecision { get; set; }

        public int? ReleaseYear => string.IsNullOrWhiteSpace(ReleaseDate[0..4]) ? null : int.Parse(ReleaseDate[0..4]);
    }
}
