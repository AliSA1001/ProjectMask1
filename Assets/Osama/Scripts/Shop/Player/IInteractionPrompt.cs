// goes next to IInteractable. PlayerInteraction shows this text while you look at the object.
// Lines are separated with \n, a line starting with '>' is the action and the UI puts the key in front of it
// e.g. "Old Jar - $10\n>Pick up"  becomes  "Old Jar - $10"  /  "[E] Pick up"
public interface IInteractionPrompt
{
    string GetPrompt();
}
