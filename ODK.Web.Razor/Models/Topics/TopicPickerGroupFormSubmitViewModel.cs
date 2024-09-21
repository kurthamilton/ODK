﻿namespace ODK.Web.Razor.Models.Topics;

public class TopicPickerGroupFormSubmitViewModel
{
    public string Name { get; set; } = "";

    public List<string> SelectedTopics { get; set; } = new();

    public List<string> Topics { get; set; } = new();
}
