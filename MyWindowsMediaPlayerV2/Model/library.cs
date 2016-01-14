using MyWindowsMediaPlayerV2.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MyWindowsMediaPlayerV2.Model
{
    [Serializable()]
    public class Library
    {
        public List<Artist> artists { get; set; }
        public List<Album> albums { get; set; }
        public List<Song> songs { get; set; }

        public Library()
        {
            artists = new List<Artist>();
            albums = new List<Album>();
            songs = new List<Song>();
        }
    }

    public class ServiceLib
    {
        public Library library { get; set; }
        public ServiceLib(Library lib)
        {
            this.library = lib;
            if (File.Exists(@"\Temp\library.xml"))
                this.loadLib(@"\Temp\library.xml");
        }
        public void addMusic(string path)
        {
            if (path != "")
            {
                FileAttributes attr = File.GetAttributes(path);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    string[] mp3 = Directory.GetFiles(path, "*.mp3");
                    string[] wma = Directory.GetFiles(path, "*.wma");
                    string[] wav = Directory.GetFiles(path, "*.wav");

                    for (int i = 0; i < mp3.Length; i++)
                        addSongInfo(mp3[i]);
                    for (int i = 0; i < wma.Length; i++)
                        addSongInfo(wma[i]);
                    for (int i = 0; i < wav.Length; i++)
                        addSongInfo(wav[i]);
                }
                else
                {
                    addSongInfo(path);
                }
                this.update(library);
                this.saveLib(library);
            }
        }
        public void addSongInfo(string path)
        {
            TagLib.File file = TagLib.File.Create(path);
            Song song = new Song( file.Tag.Title, file.Tag.Performers.ToList<string>(), file.Tag.Album, path, file.Tag.TrackCount);
            library.songs.Add(song);
        }

        public void delAlbum(Album album, bool fromComputer)
        {
            for (int i = 0; i < library.albums.Count(); i++)
            {
                if (album.name == library.albums[i].name)
                {
                    library.albums.RemoveAt(i);
                    if (fromComputer == true)
                        for (int j = 0; j < album.songs.Count(); j++)
                            if (File.Exists(album.songs[i].path))
                                File.Delete(album.songs[i].path);
                }
            }
            saveLib(library);
        }

        public void delSong(Song song, bool fromComputer)
        {
            for (int i = 0; i < library.songs.Count(); i++)
            {
                if (song.path == library.songs[i].path)
                {
                    library.songs.RemoveAt(i);
                    if (fromComputer == true && File.Exists(song.title))
                        File.Delete(song.path);
                }
            }
            saveLib(library);
        }
        private void update(Library library)
        {
            // formating songs with N/A when the field is empty
            for (int i = 0; i < this.library.songs.Count(); i++)
                this.library.songs[i] = this.library.songs[i].formatSong();
            // adding songs to albums
            for (int i = 0; i < this.library.songs.Count(); i++)
            {
                int index;
                if ((index = Search.itExist(this.library.songs[i].album, this.library.albums)) != -1)
                    this.library.albums[index].addSong(this.library.songs[i]);
                else
                    this.library.albums.Add(new Album(this.library.songs[i].album, this.library.songs[i].artist, new List<Song>()));
            }

            // adding artists to albums
            for (int i = 0; i < this.library.albums.Count(); i++)
            {
                for (int j = 0; j < this.library.albums[i].songs.Count(); j++)
                {
                    for (int k = 0; k < this.library.albums[i].songs[j].artist.Count(); k++)
                    {
                        int index;
                        if ((index = Search.itExist(this.library.albums[i].songs[j].artist[k], this.library.albums[i].artists)) == -1)
                            this.library.albums[i].artists.Add(this.library.albums[i].songs[j].artist[k]);
                    }
                }
            }

            //adding albums to artists
            for (int i = 0; i < this.library.albums.Count(); i++)
            {
                for (int j = 0; j < this.library.albums[i].artists.Count(); j++)
                {
                    int index;
                    if ((index = Search.itExist(this.library.albums[i].artists[j], this.library.artists)) == -1)
                    {
                        this.library.artists.Add(new Artist(this.library.albums[i].artists[j], new List<Album>()));
                        this.library.artists.Last().albums.Add(this.library.albums[i]);
                    }
                    else if ((Search.itExist(this.library.albums[i].name, this.library.artists[index].albums)) == -1)
                        this.library.artists[index].albums.Add(this.library.albums[i]);
                }
            }
        }

        public void loadLib(string filePath)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Library));
            using (var sr = new StreamReader(filePath))
            {
                Library lib = (Library)xs.Deserialize(sr);
                this.library = lib;
            }
        }

        public void saveLib(Library lib)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Library));
            using (TextWriter writer = new StreamWriter(@"\temp\library.xml"))
            {
                xs.Serialize(writer, lib);
            }
        }

    }

    public struct Artist
    {
        public string name { get; set; }
        public List<Album> albums { get; set; }

        public Artist(string name, List<Album> albums)
        {
            this.name = name;
            this.albums = albums;
        }

        public void addAlbum(Album album)
        {
            for (int i = 0; i < albums.Count(); i++)
                if (name == album.name)
                    albums.Add(album);
        }
    }

    public struct Album
    {
        public string name { get; set; }
        public List<string> artists { get; set; }
        public List<Song> songs { get; set; }

        public Album(string name, List<string> artists, List<Song> songs)
        {
            this.name = name;
            this.artists = artists;
            this.songs = songs;
        }

        public void addArtist(string artist)
        {
            artists.Add(artist);
        }
        public void addSong(Song song)
        {
            songs.Add(song);
        }
        
    }

    public struct Song
    {
        public string title { get; set; }
        public List<string> artist { get; set; }
        public string album { get; set; }
        public string path { get; set; }
        public uint id { get; set; }

        public Song(string title, List<string> artist, string album, string path, uint id)
        {
            this.title = title;
            this.artist = artist;
            this.album = album;
            this.path = path;
            this.id = id;
        }

        public Song formatSong()
        {
            if (album == null)
                album = "N/A";
            if (artist.Count() == 0)
                artist.Add("N/A");
            if (title == null)
                title = "N/A";
            return (new Song(title, artist, album, path, id));
        }

        
    }
}
