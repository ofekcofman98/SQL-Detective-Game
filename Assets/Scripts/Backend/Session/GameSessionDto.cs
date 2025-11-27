using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionDto
{
    public Guid id { get; set; }
    public string key { get; set; }
    public bool pcConnected { get; set; }
    public bool mobileConnected { get; set; }
    public DateTime createdAt { get; set; }
    // public GameProgressDto progress { get; set; }
}
