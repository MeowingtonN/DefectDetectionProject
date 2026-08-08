using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Core.DefectDetection.Models
{
    /// <summary>
    /// 模型训练参数实体类
    /// </summary>
    public class TrainningParam : BindableBase
    {
        /// <summary>
        /// 模型训练时，每次模型参数更新时使用的训练样本数量
        /// </summary>
        private int batchSize = 4;
        /// <summary>
        /// 模型训练时，每次模型参数更新时使用的训练样本数量
        /// </summary>
        public int BatchSize
        {
            get { return batchSize; } 
            set 
            { 
                batchSize = value; 
                if(batchSize < 1)
                {
                    batchSize = 1;
                }
                RaisePropertyChanged(); 
            }
        }

        /// <summary>
        /// 学习率
        /// </summary>
        private double learningRate = 0.0001;
        /// <summary>
        /// 学习率
        /// </summary>
        public double LearningRate
        {
            get { return learningRate; }
            set
            {
                learningRate = value;
                if(learningRate <= 0)
                {
                    learningRate = 0.0001;
                }
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 动量
        /// </summary>
        private double momentum = 0.99;
        /// <summary>
        /// 动量
        /// </summary>
        public double Momentum
        {
            get { return momentum; }
            set
            {
                momentum = value;
                if(momentum >= 1)
                {
                    momentum = 0.9;
                }
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 训练的总周期数（epoch），一个 epoch 表示完整遍历一次训练数据集
        /// </summary>
        private int numEpochs = 10;
        /// <summary>
        /// 训练的总周期数（epoch），一个 epoch 表示完整遍历一次训练数据集
        /// </summary>
        public int NumEpochs
        {
            get { return numEpochs; }
            set
            {
                numEpochs = value;
                if(numEpochs < 1)
                {
                    numEpochs = 1;
                }
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 评估（验证）间隔，单位为epoch。例如设为 1 表示每个 epoch 结束后都在验证集上计算一次评估指标；设为 5 则每 5 个 epoch 评估一次。
        /// </summary>
        private int evaluationIntervalEpochs = 1;
        /// <summary>
        /// 评估（验证）间隔，单位为epoch。例如设为 1 表示每个 epoch 结束后都在验证集上计算一次评估指标；设为 5 则每 5 个 epoch 评估一次。
        /// </summary>
        public int EvaluationIntervalEpochs
        {
            get { return evaluationIntervalEpochs; }
            set
            {
                evaluationIntervalEpochs = value;
                if(evaluationIntervalEpochs < 1)
                {
                    evaluationIntervalEpochs = 1;
                }
                RaisePropertyChanged();
            }
        }
    }
}
