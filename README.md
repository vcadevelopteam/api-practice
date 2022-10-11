# api-practice
Ejercicio práctico

Habilitar un servicio web donde se pueda obtener el siguiente resultado:

•	Teniendo en cuenta un conjunto de opciones ingresadas, donde cada opción cuenta con un nombre y un porcentaje, escoger y devolver una opción al azar considerando el porcentaje acompañante de cada opción como la probabilidad para que esta sea escogida.

•	En el caso de que se ingresen múltiples opciones con un mismo nombre, los porcentajes de las opciones repetidas se deben acumular y tratar como una sola.

•	Se debe permitir como máximo el ingreso de X opciones diferentes. Este límite debe ser definido en alguna parte del proyecto.

•	Los porcentajes de cada opción no deben ser menores a 0%.

•	La suma de todos los porcentajes no debe ser mayor a 100%. En el caso de que esta suma sea menor a 100%, se debe añadir una opción "Indeterminado" de manera interna que tome el porcentaje restante para llegar a 100%.

•	Este servicio debe estar protegido por algún tipo de autenticación. (Bearer Token, Basic, ApiKey, etc).

•	En el caso de que se incumplan algunas de las reglas colocadas anteriormente, el servicio debe devolver un código de error 500 detallando la regla que no fue cumplida.

Se debe preparar además pequeño documento donde se indiquen las instrucciones para probar el servicio construido.
