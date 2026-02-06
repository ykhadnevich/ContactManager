namespace ContactManager.Application.DTOs;

public class UploadResultDto
{
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool IsSuccess => FailedRecords == 0;
}