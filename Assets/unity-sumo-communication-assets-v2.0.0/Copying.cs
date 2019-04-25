
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Copying : MonoBehaviour {

    public int whatever;
    public int whatisthis;

    [ContextMenu("CopyFields")]
    public void CopyFields()
    {
        var type = typeof(Tilemap);

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Debug.Log("You copied " + fields.Length + " fields");
        foreach(var field in fields)
        {
            var sb = new StringBuilder("Type");

            sb.Append(field.GetType().ToString());
            sb.Append(" Name : ");
            sb.Append(field.Name);
        }

        Debug.Log("You copied " + properties.Length + " properties");
        foreach (var property in properties)
        {
            var sb = new StringBuilder("Type");

            sb.Append(property.GetType().ToString());
            sb.Append(" Name : ");
            sb.Append(property.Name);
            
            Debug.Log(sb.ToString() );
        }

    }
}
