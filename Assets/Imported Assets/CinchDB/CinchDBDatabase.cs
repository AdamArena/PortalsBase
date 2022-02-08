using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores data related to a CinchDB database.
/// </summary>
public class CinchDBDatabase : ScriptableObject
{
    // The key this database connects to.
    public string key;

    // The number of columns used by this database. This doesn't affect functionality,
    // but does limit the number of columns shown in the editor.
    [Range(1, 10)]
    public int visibleColumns = 5;

    /// <summary>
    /// The database column headers.
    /// </summary>
    public string[] ColumnHeaders = new string[]
    {
        "Column 1",
        "Column 2",
        "Column 3",
        "Column 4",
        "Column 5",
        "Column 6",
        "Column 7",
        "Column 8",
        "Column 9",
        "Column 10"
    };

    [HideInInspector]
    public CinchDBColumn sortColumn = CinchDBColumn.None;
    public bool sortAscending = true;
}
