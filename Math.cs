using System;
using System.Collections.Generic;
using System.Text;

namespace SAPRamat
{
    class Math
    {
        public static double[] Gauss(double[,] Matrix)
        {
            int n = Matrix.GetLength(0); //Размерность начальной матрицы (строки)
            double[,] Matrix_Clone = new double[n, n + 1]; //Матрица-дублер
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n + 1; j++)
                    Matrix_Clone[i, j] = Matrix[i, j];

            //Прямой ход (Зануление нижнего левого угла)
            for (int k = 0; k < n; k++) //k-номер строки
            {
                for (int i = 0; i < n + 1; i++) //i-номер столбца
                    Matrix_Clone[k, i] = Matrix_Clone[k, i] / Matrix[k, k]; //Деление k-строки на первый член !=0 для преобразования его в единицу
                for (int i = k + 1; i < n; i++) //i-номер следующей строки после k
                {
                    double K = Matrix_Clone[i, k] / Matrix_Clone[k, k]; //Коэффициент
                    for (int j = 0; j < n + 1; j++) //j-номер столбца следующей строки после k
                        Matrix_Clone[i, j] = Matrix_Clone[i, j] - Matrix_Clone[k, j] * K; //Зануление элементов матрицы ниже первого члена, преобразованного в единицу
                }
                for (int i = 0; i < n; i++) //Обновление, внесение изменений в начальную матрицу
                    for (int j = 0; j < n + 1; j++)
                        Matrix[i, j] = Matrix_Clone[i, j];
            }

            //Обратный ход (Зануление верхнего правого угла)
            for (int k = n - 1; k > -1; k--) //k-номер строки
            {
                for (int i = n; i > -1; i--) //i-номер столбца
                    Matrix_Clone[k, i] = Matrix_Clone[k, i] / Matrix[k, k];
                for (int i = k - 1; i > -1; i--) //i-номер следующей строки после k
                {
                    double K = Matrix_Clone[i, k] / Matrix_Clone[k, k];
                    for (int j = n; j > -1; j--) //j-номер столбца следующей строки после k
                        Matrix_Clone[i, j] = Matrix_Clone[i, j] - Matrix_Clone[k, j] * K;
                }
            }

            //Отделяем от общей матрицы ответы
            double[] Answer = new double[n];
            for (int i = 0; i < n; i++)
                Answer[i] = Matrix_Clone[i, n];

            return Answer;
        }
    }
}
