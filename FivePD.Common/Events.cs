namespace FivePD.Common
{
    /**
     * Contains constant definitions for event handler names
     */
    public abstract class Events
    {
        // Follows naming convention "{resource}:{area}:{id}" OR "{resource}:{area}:{id}:{type}" where,
        // {resource} is "fivepd"
        // {area} is a camelCased descriptor of the applicable event location
        // {id} is a camelCased descriptor of the event itself
        // {type} is optional, either "server" or "client" 
        
        public abstract class EntityManagement
        {
            /**
             * C->S: Used when a client wants to request that the server creates a ped.
             */
            public const string RequestPed = "fivepd:entityManagement:requestPed";
            
            /**
             * C->S: Used when a client wants to request that the server creates a ped in an existing vehicle.
             */
            public const string RequestPedInVehicle = "fivepd:entityManagement:requestPedInVehicle";
            
            /**
             * C->S: Used when a client wants to request that the server creates a vehicle.
             */
            public const string RequestVehicle = "fivepd:entityManagement:requestVehicle";
            
            /**
             * C->S: Used when a client wants to request that the server creates a prop (object).
             */
            public const string RequestProp = "fivepd:entityManagement:requestProp";
        }
    }
}