import { useEffect, useState } from "react";

type Address = {
  id: number;
  label: string;
  street: string;
  city: string;
  isDefault: boolean;
};

export default function AddressList() {
  const [addresses, setAddresses] = useState<Address[]>([]);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    fetch("/api/addresses")
      .then((res) => {
        if (!res.ok) throw new Error("Could not load addresses");
        return res.json();
      })
      .then(setAddresses)
      .catch(() => setError("Could not load addresses"));
  }, []);

  async function makeDefault(id: number) {
    setMessage("");
    setError("");
    try {
      const response = await fetch(`/api/addresses/${id}/default`, { method: "PUT" });
      if (!response.ok) throw new Error("Could not update default address");
      setAddresses((current) => current
        .map((a) => ({ ...a, isDefault: a.id === id }))
        .sort((a, b) => Number(b.isDefault) - Number(a.isDefault) || a.label.localeCompare(b.label)));
      setMessage("Default address updated");
    } catch {
      setError("Could not update default address");
    }
  }

  async function deleteAddress(id: number) {
    setMessage("");
    setError("");
    try {
      const response = await fetch(`/api/addresses/${id}`, { method: "DELETE" });
      if (!response.ok) throw new Error("Could not delete address");
      setAddresses((current) => current.filter((a) => a.id !== id));
      setMessage("Address deleted");
    } catch {
      setError("Could not delete address");
    }
  }

  return (
    <main>
      <h1>My addresses</h1>
      <p>Signed in as customer 1</p>
      {error && <p role="alert">{error}</p>}
      {message && <p role="status">{message}</p>}
      <table>
        <thead>
          <tr>
            <th>Label</th>
            <th>Street</th>
            <th>City</th>
            <th>Default</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {addresses.map((a) => (
            <tr key={a.id}>
              <td>{a.label}</td>
              <td>{a.street}</td>
              <td>{a.city}</td>
              <td>{a.isDefault ? "Default" : ""}</td>
              <td>
                {!a.isDefault && <button onClick={() => makeDefault(a.id)}>Make default</button>}
                <button onClick={() => deleteAddress(a.id)}>Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </main>
  );
}
