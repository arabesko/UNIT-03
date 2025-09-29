using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProjectorLine
{
    [TextArea(2, 6)] public string text;
    public AudioClip voiceClip;            // Sonido al iniciar esta línea (puede ser null)
    public float autoAdvanceAfter = 0f;    // si >0 avanza automáticamente tras N segundos desde que termina el typewriter. 0 => espera input
}

[Serializable]
public class ProjectorDialogue
{
    public string id;                      // identificador (útil para llamar por nombre)
    public List<ProjectorLine> lines = new List<ProjectorLine>();
    public bool closeWhenDone = true;      // vuelve a posición por defecto al terminar
}
