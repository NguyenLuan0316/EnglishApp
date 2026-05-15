using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWave.Domain.Models;

[Table("toeic_import_logs")]
public class ToeicImportLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("source_type")]
    public string SourceType { get; set; } = "";

    [Column("source_name")]
    public string SourceName { get; set; } = "";

    [Column("source_url")]
    public string SourceUrl { get; set; } = "";

    [Column("status")]
    public string Status { get; set; } = "";

    [Column("total_items")]
    public int TotalItems { get; set; }

    [Column("imported_items")]
    public int ImportedItems { get; set; }

    [Column("failed_items")]
    public int FailedItems { get; set; }

    [Column("error_message")]
    public string ErrorMessage { get; set; } = "";

    [Column("details")]
    public string Details { get; set; } = "";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
