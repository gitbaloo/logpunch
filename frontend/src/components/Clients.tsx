import React, { useState, useEffect } from "react";
import SearchBar from "../components/SearchBar";
import { Client } from "../types/genericTypes";
import { fetchClients } from "../services/apiService";

const Clients: React.FC = () => {
  const [clients, setClients] = useState<Client[]>([]);
  const [filteredClients, setFilteredClients] = useState<Client[]>([]);
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);

  useEffect(() => {
    const getClients = async () => {
      try {
        const data: Client[] = await fetchClients();
        setClients(data);
        setFilteredClients(data);
      } catch (error) {
        console.error("Failed to fetch clients:", error);
      }
    };

    getClients();
  }, []);

  useEffect(() => {
    const results = clients.filter((client) =>
      client.name.toLowerCase().includes(searchQuery.toLowerCase())
    );
    setFilteredClients(results);
  }, [searchQuery, clients]);

  const handleClientClick = (client: Client) => {
    setSelectedClient(client);
  };

  return (
    <div>
      <SearchBar searchQuery={searchQuery} setSearchQuery={setSearchQuery} />
      <ul>
        {filteredClients.map((client) => (
          <li key={client.id}>
            <button onClick={() => handleClientClick(client)}>
              {client.name}
            </button>
            {selectedClient && selectedClient.id === client.id}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Clients;
