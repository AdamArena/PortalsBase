using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using System;
using UnityEditorInternal;
using System.Threading.Tasks;
using System.Linq;

/// <summary>
/// The CinchDBEditor class manages the editor window for adding, 
/// editing and removing database information.
/// </summary>
public class CinchDBEditor : EditorWindow
{
    public int VISIBLE_COLUMN_COUNT = 5;

    // Reference to the window currently being displayed (null if not visible).
    private static EditorWindow window;

    // The current database being displayed.
    private static CinchDBDatabase selectedDatabase = null;

    // Reference to all records in the current database.
    private static List<CinchDBRecord> records = new List<CinchDBRecord>();

    // True if the database is currently loading data, otherwise false.
    private static bool loadingData;

    // Displays the current log message to the user.
    private static string logMessage = "";
    private static Color logMessageColor = Color.white;

    private static int sortColumn = -1;
    private static bool sortAsc = true;
    private static Vector2 scrollPosition = Vector2.zero;


    /// <summary>
    /// Open the editor window.
    /// </summary>
    [MenuItem("CinchDB/Open CinchDB Editor")]
    public static async void ShowWindow ()
    {
        window = EditorWindow.GetWindow(typeof(CinchDBEditor));
        window.titleContent.text = "Cinch DB Editor";
        scrollPosition = Vector2.zero;
        CinchDBDatabase[] databases = Resources.LoadAll<CinchDBDatabase>("");
        if (databases.Length > 0)
        {
            selectedDatabase = databases[0];
            OnRefreshDatabaseClicked();
        }
        else
        {
            await GenerateNewDatabase();
        }
    }


    /// <summary>
    /// Ensures that the window is updated every frame, even if it is not in focus.
    /// </summary>
    private void Update()
    {
        Repaint();
    }


    /// <summary>
    /// Have the system generate a new database and save it in the correct location.
    /// </summary>
    public static async Task GenerateNewDatabase ()
    {
        SetMessage("Creating Database...", Color.white);
        try
        {
            string path = "Assets/Database.asset";

            // Find the marker to determine where the database should be created.
            if (AssetDatabase.FindAssets("t:" + typeof(CinchDBDatabase).Name).Length > 0)
            {
                string guid = AssetDatabase.FindAssets("t:" + typeof(CinchDBDatabase).Name)[0];
                CinchDBDatabase marker = AssetDatabase.LoadAssetAtPath<CinchDBDatabase>(AssetDatabase.GUIDToAssetPath(guid));

                // Generate a placeholder name for the new database.
                path = AssetDatabase.GUIDToAssetPath(guid).Replace(marker.name, "Database");
                path = AssetDatabase.GenerateUniqueAssetPath(path);
            }

            // Create the database with a unique key.
            string databaseKey = await CinchDB.GetDatabaseKey();
            CinchDBDatabase asset = ScriptableObject.CreateInstance<CinchDBDatabase>();
            asset.key = databaseKey;

            // Save the database asset.
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            // Load the new database.
            selectedDatabase = asset;
            OnRefreshDatabaseClicked();
            SetMessage("Successfully Created Database.", Color.green);
        }
        catch
        {
            SetMessage("Connection issue. Unable to create new database.", Color.red);
        }
    }


    /// <summary>
    /// if the scripts are being compiled and no database key has been generated, create one.
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    public static async void OnScriptsReloaded ()
    {
        CinchDBDatabase[] databases = Resources.LoadAll<CinchDBDatabase>("");
        if (databases.Length == 0)
        {
            await GenerateNewDatabase();
        }
        else
        {
            selectedDatabase = databases[0];
            OnRefreshDatabaseClicked();
        }
    }


    /// <summary>
    /// Creates a completely new database for the user.
    /// </summary>
    public static async void OnCreateDatabaseClicked ()
    {
        await GenerateNewDatabase();
    }


    /// <summary>
    /// Delete the selected database.
    /// </summary>
    public static void OnDeleteDatabaseClicked ()
    {
        SetMessage("Deleting Database...", Color.white);
        string databasePath = AssetDatabase.GetAssetPath(selectedDatabase);
        AssetDatabase.DeleteAsset(databasePath);
        selectedDatabase = Resources.LoadAll<CinchDBDatabase>("")[0];
        OnRefreshDatabaseClicked();
        SetMessage("Successfully Deleted Database.", Color.green);
    }


    /// <summary>
    /// Button Callback: Deletes ALL data in the selected database. Use with caution!
    /// </summary>
    public static async void OnResetDatabaseClicked ()
    {
        SetMessage("Clearing Database Records...", Color.white);
        try
        {
            records = new List<CinchDBRecord>();
            await CinchDB.ClearAllRecords(selectedDatabase);
            SetMessage("Successfully Cleared Database.", Color.green);
        }
        catch
        {
            SetMessage("Connection issue. Unable to clear database records.", Color.red);
        }
    }


    /// <summary>
    /// Button Callback: Refresh the database information.
    /// </summary>
    public static async void OnRefreshDatabaseClicked ()
    {
        SetMessage("Reloading Database...", Color.white);
        loadingData = true;
        records = new List<CinchDBRecord>();
        try { 
            records = await CinchDB.GetAllRecords(selectedDatabase);
            loadingData = false;
            SetMessage("Successfully Reloaded Database.", Color.green);
        }
        catch
        {
            SetMessage("Connection issue. Unable to refresh database records.", Color.red);
        }
    }


    /// <summary>
    /// Button Callback: Adds a new item to the selected database (will not be uploaded until saved).
    /// </summary>
    public static void OnAddDatabaseItemClicked ()
    {
        CinchDBRecord record = new CinchDBRecord();
        record.IsEditing = true;
        records.Add(record);
    }


    /// <summary>
    /// Button Callback: Updates the specified record from the specified database.
    /// </summary>
    public static async void OnSaveDatabaseRecordPressed (CinchDBDatabase database, CinchDBRecord record)
    {
        SetMessage("Saving Record...", Color.white);
        record.IsEditing = false;
        try
        {
            if (record.RecordID.Length > 0)
            {
                await CinchDB.UpdateRecords(database, record);
            }
            else
            {
                await CinchDB.AddRecords(database, record);
            }
            SetMessage("Saved Database Record.", Color.green);
        }
        catch
        {
            SetMessage("Connection issue. Unable to update database record.", Color.red);
        }
    }


    /// <summary>
    /// Button Callback: Deletes the specified record from the specified database.
    /// </summary>
    public static async void OnDeleteDatabaseRecordPressed (CinchDBDatabase database, CinchDBRecord record)
    {
        SetMessage("Deleting Record...", Color.white);
        try
        {
            await CinchDB.DeleteRecords(database, record);
            records.Remove(record);
            SetMessage("Deleted Database Record.", Color.green);
        }
        catch
        {
            SetMessage("Connection issue. Unable to delete database record.", Color.red);
        }
    }


    /// <summary>
    /// Sets the log message for the panel to be shown in the specified color.
    /// </summary>
    private static void SetMessage(string message, Color messageColor)
    {
        logMessage = message;
        logMessageColor = messageColor;
    }

    private static async void Test ()
    {
        /*
        await CinchDB.DeleteAllRecordsMatching(selectedDatabase, 
            new CinchDBDataCondition(CinchDBDataColumn.Column1, CinchDBEvaluator.EqualTo, "does"), 
            new CinchDBDataCondition(CinchDBDataColumn.Column2, CinchDBEvaluator.NotEqualTo, "thisy"));
        OnRefreshDatabaseClicked();
        */

        //await CinchDB.GetAllRecordsMatching(selectedDatabase, new CinchDBDataOrder(CinchDBDataOrderType.Descending, CinchDBDataColumn.Column1), 10, new CinchDBDataCondition(CinchDBDataColumn.Column2, CinchDBEvaluator.EqualTo, "Test"));

        /*
        CinchDBColumnValue[] values = new CinchDBColumnValue[]
        {
            new CinchDBColumnValue(CinchDBDataColumn.Column1, "0"),
            new CinchDBColumnValue(CinchDBDataColumn.Column2, "TEST")
        };
        await CinchDB.UpdateAllRecordsMatching(selectedDatabase, values, new CinchDBDataCondition(CinchDBDataColumn.Column1, CinchDBEvaluator.LessThan, "5"));
        */

        /*
        CinchDBRetrieveDataRequest request = new CinchDBRetrieveDataRequest(selectedDatabase);
        request.AddCondition(CinchDBDataColumn.Column1, CinchDBEvaluator.EqualTo, "5");
        request.SetLimit(10);
        request.SetOrder(true, CinchDBDataColumn.Column1);
        await request.ExecuteRequest();
        */

        /*
        CinchDBDeleteDataRequest request = new CinchDBDeleteDataRequest(selectedDatabase);
        request.AddCondition(CinchDBDataColumn.Column1, CinchDBEvaluator.EqualTo, "5");
        request.ExecuteRequest();
        */

        /*
        CinchDBUpdateDataRequest request = new CinchDBUpdateDataRequest(selectedDatabase);
        request.AddCondition(CinchDBColumn.Column1, CinchDBEvaluator.EqualTo, "0");
        request.AddUpdatedValue(CinchDBColumn.Column1, "100");
        await request.ExecuteRequest();
        */

        //await CinchLeaderboard.AddScore("Dave", 500.ToString());
        //Debug.Log (await CinchLeaderboard.GetHighestScoreByPlayer("Joel"));
        List<CinchScore> scores = await CinchLeaderboard.GetScores(10);
        foreach (CinchScore score in scores)
        {
            Debug.Log(score.name + ": " + score.score);
        }
    }

    /// <summary>
    /// Draw the main editor window.
    /// Code is rather messy, but won't be included in builds.
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        // Draw the top toolbar.
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Create Database", EditorStyles.toolbarButton, GUILayout.Width(125)))
            OnCreateDatabaseClicked();
        if (GUILayout.Button("Delete Database", EditorStyles.toolbarButton, GUILayout.Width(125)))
            OnDeleteDatabaseClicked();
        if (GUILayout.Button("Clear Database", EditorStyles.toolbarButton, GUILayout.Width(125)))
            OnResetDatabaseClicked();
        if (GUILayout.Button("Test", EditorStyles.toolbarButton, GUILayout.Width(125)))
            Test();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh Data", EditorStyles.toolbarButton, GUILayout.Width(100)))
            OnRefreshDatabaseClicked();
        if (GUILayout.Button("Add Item", EditorStyles.toolbarButton, GUILayout.Width(100)))
            OnAddDatabaseItemClicked();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(10));
        int selectedDatabaseID = 0;
        CinchDBDatabase[] databases = Resources.LoadAll<CinchDBDatabase>("");
        string[] databaseNames = new string[databases.Length];
        for (int i = 0; i < databases.Length; i++)
        {
            if (databases[i] == selectedDatabase)
            {
                selectedDatabaseID = i;
            }
            databaseNames[i] = databases[i].name;
        }
        EditorGUILayout.LabelField("Selected Database:", EditorStyles.boldLabel, GUILayout.Width(130));
        int newSelectedID = EditorGUILayout.Popup(selectedDatabaseID, databaseNames, GUILayout.Width(200));
        if (newSelectedID != selectedDatabaseID)
        {
            selectedDatabase = databases[newSelectedID];
            sortColumn = -1;
            OnRefreshDatabaseClicked();
        }
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Draw the database loading message if content is being collected.
        if (loadingData)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            EditorGUILayout.LabelField("Loading Data...");
            EditorGUILayout.EndHorizontal();
        }
        else if (records.Count == 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            EditorGUILayout.LabelField("The database is empty.");
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.EndVertical();
        }

        if (records.Count > 0)
        {

            // Draw the database column headers.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            for (int i = 0; i < selectedDatabase.visibleColumns; i++)
            {
                string extra = "";
                if (sortColumn == i && sortAsc) extra = " ↑";
                else if (sortColumn == i && !sortAsc) extra = " ↓";
                if (GUILayout.Button(selectedDatabase.ColumnHeaders[i] + extra, style, GUILayout.Width(120)))
                {
                    if (sortColumn != i)
                    {
                        sortColumn = i;
                        sortAsc = true;
                    }
                    else
                    {
                        sortAsc = !sortAsc;
                    }

                    if (sortColumn != -1)
                    {
                        if (sortAsc)
                            records = records.OrderBy(p => p.Columns[i]).ToList();
                        else
                            records = records.OrderByDescending(p => p.Columns[i]).ToList();
                    }
                    
                }
                // EditorGUILayout.LabelField(selectedDatabase.ColumnHeaders[i], style, GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            // Draw the data associated with each record.
            for (int j = 0; j < records.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(10));
                for (int i = 0; i < selectedDatabase.visibleColumns; i++)
                {
                    EditorGUI.BeginDisabledGroup(!records[j].IsEditing);
                    records[j].Columns[i] = EditorGUILayout.TextField(records[j].Columns[i], GUILayout.Width(120));
                    EditorGUI.EndDisabledGroup();
                }
                if (records[j].IsEditing)
                {
                    if (GUILayout.Button("Save", GUILayout.Width(75)))
                        OnSaveDatabaseRecordPressed(selectedDatabase, records[j]);
                } 
                else
                {
                    if (GUILayout.Button("Edit", GUILayout.Width(75)))
                        records[j].IsEditing = true;
                }
                    
                if (GUILayout.Button("Delete", GUILayout.Width(75)))
                    OnDeleteDatabaseRecordPressed(selectedDatabase, records[j]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        // Draw the bottom toolbar.
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUIStyle bottomStyle = new GUIStyle(EditorStyles.toolbarButton);
        bottomStyle.alignment = TextAnchor.MiddleLeft;
        GUI.backgroundColor = logMessageColor;
        GUI.color = logMessageColor;
        GUILayout.Label(logMessage, bottomStyle, GUILayout.Width(10000));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
}



