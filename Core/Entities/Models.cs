using System;
using System.Collections.Generic;

namespace practiceStudioAssetManager.Core.Entities;

public enum EquipmentCategory { Microphone, Synthesizer, OutboardGear, AudioInterface, Cables, Other }
public enum LicenseType { Perpetual, Subscription, iLok, eLicenser, OpenSource }
public enum EquipmentStatus { Available, InUse, InMaintenance, Retired }

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Hardware : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public EquipmentCategory Category { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;

    public List<StudioSession> Sessions { get; set; } = new();

    public override string ToString() => $"{Name} (S/N: {SerialNumber})";
}

public class Software : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Developer { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public string AuthMethod { get; set; } = string.Empty;
    public string Workstation { get; set; } = "Studio PC A"; 
}

public class Engineer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<StudioSession> Sessions { get; set; } = new();

    public override string ToString() => $"{FullName} | {Role}";
}

public class Client : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public override string ToString() => FullName;
}

public class StudioSession : BaseEntity
{
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    
    public List<Hardware> Equipments { get; set; } = new();
    public List<Engineer> Engineers { get; set; } = new();
    
    public string ProjectName { get; set; } = string.Empty; 
    public string Notes { get; set; } = string.Empty; 
    
    public DateTime CheckedOutAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnedAt { get; set; }
    
    public bool IsActive => ReturnedAt == null;
}

public class HardwareDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class SoftwareDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Developer { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public string AuthMethod { get; set; } = string.Empty;
    public string Workstation { get; set; } = string.Empty;
}

public class EngineerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ClientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class SessionViewDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public int HardwareCount { get; set; }
    public int EngineersCount { get; set; }
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}