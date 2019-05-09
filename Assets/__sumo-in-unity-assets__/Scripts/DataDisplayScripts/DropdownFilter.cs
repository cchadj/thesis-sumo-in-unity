using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownFilter : MonoBehaviour
{
    [SerializeField]
    private InputField inputField;

    [SerializeField]
    private Dropdown dropdown;

    private List<Dropdown.OptionData> dropdownOptions;

    public Dropdown Dropdown { get => dropdown; set => dropdown = value; }
    public List<Dropdown.OptionData> DropdownOptions { get => dropdownOptions; set => dropdownOptions = value; }

    private void Start()
        {
            DropdownOptions = Dropdown.options;
        }
    public void FilterDropdown(string input)
    {
     
        Dropdown.options = DropdownOptions.FindAll(option => option.text.IndexOf(input) >= 0);
    }
    

}
