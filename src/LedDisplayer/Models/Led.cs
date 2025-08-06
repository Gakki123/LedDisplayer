using System.ComponentModel.DataAnnotations;

namespace LedDisplayer.Models;

public class Led
{
    [Required]
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Ip { get; set; }
}
