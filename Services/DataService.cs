using LiteDB;
using mynydd.Models;

namespace mynydd.Services;

public class DataStore
{
    const string name = "notes";

    private readonly string connection = $"Filename={name}.db;Connection=Shared";

    public List<Note> GetAllNotes()
    {
        using var db = new LiteDatabase(connection);
        var collection = db.GetCollection<Note>(name);
        return collection.FindAll()
                        .OrderByDescending(i => i.TimestampUtc)
                        .ToList();
    }

    public void InsertNote(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;  
        } 

        using var db = new LiteDatabase(connection);

        var collection = db.GetCollection<Note>(name);

        // Enforce limit of 5 notes - delete oldest notes if we have more than 4
        var currentNotes = collection.FindAll()
                                    .OrderBy(n => n.TimestampUtc)
                                    .ToList();
        
        while (currentNotes.Count > 4)
        {
            collection.Delete(currentNotes[0].Id);
            currentNotes.RemoveAt(0);
        }

        collection.Insert(new Note
        {
            Text = text,
            TimestampUtc = DateTime.UtcNow
        });
    }

    public bool UpdateNote(int id, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) 
        {
            return false;
        }

        using var db = new LiteDatabase(connection);
        var collection = db.GetCollection<Note>(name);

        var note = collection.FindById(id);
        if (note is null) 
        {
            return false;
        }

        note.Text = text;

        return collection.Update(note);
    }

    public void DeleteNote(int id)
    {
        using var db = new LiteDatabase(connection);
        var collection = db.GetCollection<Note>(name);

        collection.Delete(id);
    }
}