using System;
using System.Collections.Generic;

namespace PizzeriaBellaItalia.Dominio;

// Clase de apoyo para mantener pedidos en memoria (no es la fuente principal de datos).
// En esta versión la persistencia real se realiza en SQL Server mediante PedidoDAO;
// esta clase puede usarse para escenarios de prueba o almacenamiento temporal en RAM.
public class GestionPedidos
{
    // Colección en memoria de pedidos. Se reinicia cada vez que se crea una nueva instancia/clase en ejecución.
    public List<PedidoPizza> pedidos = [];

    // Agrega un pedido a la lista en memoria.
    public void AgregarPedido(PedidoPizza pedido)
    {
        pedidos.Add(pedido);
    }
}
